import asyncio
import json
import os
from datetime import datetime, timezone

import discord
from discord import ForumChannel, Embed, AllowedMentions
from discord.ext import commands
from dotenv import load_dotenv
from flask import Flask, jsonify, request
import gspread
from oauth2client.service_account import ServiceAccountCredentials
import threading

load_dotenv()

BOT_TOKEN = os.getenv("DISCORD_TOKEN")
FORUM_CHANNEL_ID = os.getenv("FORUM_CHANNEL_ID")
PORT = int(os.getenv("PORT", 5000))

if not BOT_TOKEN:
    raise SystemExit("Missing DISCORD_TOKEN in environment")

if not FORUM_CHANNEL_ID:
    raise SystemExit("Missing FORUM_CHANNEL_ID in environment")

try:
    FORUM_CHANNEL_ID = int(FORUM_CHANNEL_ID)
except ValueError:
    raise SystemExit("FORUM_CHANNEL_ID must be an integer")


def load_moderators() -> dict:
    """Load moderators from moderators.json file."""
    try:
        with open("moderators.json", "r") as f:
            return json.load(f)
    except FileNotFoundError:
        print("⚠️  moderators.json not found, using empty moderator list")
        return {}
    except json.JSONDecodeError:
        print("⚠️  moderators.json is invalid JSON, using empty moderator list")
        return {}
    except Exception as e:
        print(f"⚠️  Error loading moderators.json: {e}")
        return {}


MOD_MAP = load_moderators()
# Reverse lookup for Discord ID -> Roblox username
REVERSE_MOD_MAP = {str(value): key for key, value in MOD_MAP.items()}

# Google Sheets setup
try:
    creds = ServiceAccountCredentials.from_json_keyfile_name(
        "roblox-ban-bridge-237091c9c222.json",
        [
            "https://spreadsheets.google.com/feeds",
            "https://www.googleapis.com/auth/drive",
        ],  # type: ignore
    )
    client = gspread.authorize(creds)
    sheet = client.open_by_key("1WeidgdFFwXvBltFMpGfrJQd9qu7PT5WEQPr1wLzYi2M")
    print("✅ Google Sheets connected")
except Exception as e:
    print(f"❌ Google Sheets setup failed: {e}")
    sheet = None

REQUIRED_FIELDS = {"targetName", "targetId", "reason", "duration", "bannedBy"}

intents = discord.Intents.default()
intents.message_content = True
bot = commands.Bot(command_prefix="!", intents=intents)
app = Flask(__name__)


@bot.event
async def on_ready():
    print(f"✅ Bot logged in as {bot.user}")
    try:
        await bot.tree.sync()
        print("✅ Slash commands synced")
    except Exception as e:
        print(f"⚠️ Slash command sync failed: {e}")


def build_mod_mention(banned_by: str) -> str:
    discord_id = MOD_MAP.get(banned_by)
    if discord_id:
        return f"<@{discord_id}>"
    return banned_by


def find_leaderboard_row(
    moderator_name: str | None, discord_user: discord.User | None = None
):
    if sheet is None:
        return None, None

    try:
        leaderboard_ws = sheet.worksheet("Leaderboard")
        col_a = leaderboard_ws.col_values(1)
        col_b = leaderboard_ws.col_values(2)

        if moderator_name:
            if moderator_name in col_a:
                row = col_a.index(moderator_name) + 1
                return row, moderator_name
            if moderator_name in col_b:
                row = col_b.index(moderator_name) + 1
                return row, col_a[row - 1] if len(col_a) >= row else moderator_name

        if discord_user is not None:
            discord_name = discord_user.name
            discord_tag = f"{discord_user.name}#{discord_user.discriminator}"
            discord_id = str(discord_user.id)

            if discord_name in col_b:
                row = col_b.index(discord_name) + 1
                return row, col_a[row - 1] if len(col_a) >= row else None
            if discord_tag in col_b:
                row = col_b.index(discord_tag) + 1
                return row, col_a[row - 1] if len(col_a) >= row else None
            if discord_id in col_b:
                row = col_b.index(discord_id) + 1
                return row, col_a[row - 1] if len(col_a) >= row else None
    except Exception as e:
        print(f"⚠️ Leaderboard lookup failed: {e}")

    return None, None


def append_audit_log(
    timestamp: str,
    moderator: str,
    target_name: str,
    target_id: str,
    duration: str,
    reason: str,
    thread_url: str,
):
    if sheet is None:
        return

    try:
        audit_ws = sheet.worksheet("Audit Log")
        audit_ws.append_row(
            [timestamp, moderator, target_name, target_id, duration, reason, thread_url]
        )
    except Exception as e:
        print(f"❌ Google Sheets audit append failed: {e}")


def update_leaderboard_for_moderator(row: int):
    if sheet is None or row is None:
        return

    try:
        leaderboard_ws = sheet.worksheet("Leaderboard")
        current_points = int(leaderboard_ws.cell(row, 5).value or 0)
        leaderboard_ws.update_cell(row, 5, current_points + 1)
        last_log_date = datetime.now(timezone.utc).strftime("%d/%m/%y")
        leaderboard_ws.update_cell(row, 8, last_log_date)
    except Exception as e:
        print(f"❌ Google Sheets leaderboard update failed: {e}")


@bot.tree.command(
    name="log", description="Log a ban to the audit sheet and leaderboard"
)
@discord.app_commands.describe(
    target_name="Roblox username of the banned player",
    target_id="Roblox user ID of the banned player",
    duration="Ban duration, e.g. 3d or permanent",
    reason="Reason for the ban",
    moderator="Optional Roblox moderator username (uses your linked account if omitted)",
)
async def log(
    interaction: discord.Interaction,
    target_name: str,
    target_id: str,
    duration: str,
    reason: str,
    moderator: str | None = None,
):
    await interaction.response.defer(ephemeral=True)

    moderator_name = moderator
    if moderator_name is None:
        moderator_name = REVERSE_MOD_MAP.get(str(interaction.user.id))

    row, resolved_name = find_leaderboard_row(moderator_name, interaction.user)
    if resolved_name is None and moderator_name is not None:
        resolved_name = moderator_name
    if resolved_name is None:
        resolved_name = interaction.user.name

    timestamp = datetime.now(timezone.utc).isoformat()
    thread_url = "Manual /log entry"
    append_audit_log(
        timestamp, resolved_name, target_name, target_id, duration, reason, thread_url
    )

    if row is not None:
        update_leaderboard_for_moderator(row)
        await interaction.followup.send(
            f"✅ Logged ban for **{target_name}** under moderator **{resolved_name}** and updated leaderboard.",
            ephemeral=True,
        )
    else:
        await interaction.followup.send(
            f"✅ Logged ban for **{target_name}** under moderator **{resolved_name}**."
            " I could not find the moderator in the Leaderboard sheet, so points were not updated.",
            ephemeral=True,
        )


def validate_payload(payload: dict) -> tuple[bool, str]:
    if not isinstance(payload, dict):
        return False, "Payload must be a JSON object"

    missing = REQUIRED_FIELDS - payload.keys()
    if missing:
        return False, f"Missing required fields: {', '.join(sorted(missing))}"

    return True, ""


@app.route("/ban-log", methods=["POST"])
def ban_log():
    payload = request.get_json(silent=True)
    if payload is None:
        return jsonify({"success": False, "error": "Invalid JSON payload"}), 400

    valid, error_message = validate_payload(payload)
    if not valid:
        return jsonify({"success": False, "error": error_message}), 400

    if not bot.is_ready():
        return jsonify({"success": False, "error": "Bot is not ready yet"}), 503

    bot.loop.call_soon_threadsafe(asyncio.create_task, create_forum_post(payload))
    return jsonify({"success": True}), 200


async def create_forum_post(data: dict) -> None:
    target_name = data.get("targetName")
    target_id = data.get("targetId")
    reason = data.get("reason")
    duration = data.get("duration")
    banned_by = data.get("bannedBy")
    mod_mention = build_mod_mention(banned_by)

    forum_channel = bot.get_channel(FORUM_CHANNEL_ID)
    if forum_channel is None:
        try:
            forum_channel = await bot.fetch_channel(FORUM_CHANNEL_ID)
        except discord.NotFound:
            print(f"❌ Forum channel not found: {FORUM_CHANNEL_ID}")
            return
        except discord.DiscordException as exc:
            print(f"❌ Failed to fetch forum channel: {exc}")
            return

    if not isinstance(forum_channel, ForumChannel):
        print(f"❌ Channel {FORUM_CHANNEL_ID} is not a forum channel")
        return

    thread_name = f"Ban Case for user: {target_name}"
    embed = Embed(
        title="Ban Case Logged",
        color=0xE74C3C,
        timestamp=datetime.now(timezone.utc),
    )
    embed.add_field(name="Target", value=f"{target_name} (`{target_id}`)", inline=False)
    embed.add_field(name="Reason", value=reason, inline=False)
    embed.add_field(name="Duration", value=duration, inline=True)
    embed.add_field(name="Banned By", value=banned_by, inline=True)
    embed.set_footer(text="Roblox → Discord ban bridge")

    thread_content = (
        f"{mod_mention} \n" f"A new ban has been logged for **{target_name}**."
    )

    try:
        thread_result = await forum_channel.create_thread(
            name=thread_name,
            content=thread_content,
            embed=embed,
            auto_archive_duration=1440,
            allowed_mentions=AllowedMentions(
                users=(
                    [discord.Object(id=MOD_MAP[banned_by])]
                    if banned_by in MOD_MAP
                    else []
                )
            ),
        )
    except discord.DiscordException as exc:
        print(f"❌ Failed to create thread: {exc}")
        return

    thread_id = getattr(thread_result.thread, "id", None)
    if thread_id is None:
        print(f"✅ Created forum thread for ban case {target_name}")
    else:
        print(f"✅ Created forum thread {thread_id} for ban case {target_name}")

    # Google Sheets integration
    if sheet is not None:
        try:
            guild_id = forum_channel.guild.id
            thread_url = (
                f"https://discord.com/channels/{guild_id}/{thread_result.thread.id}"
            )

            # Audit Log
            audit_ws = sheet.worksheet("Audit Log")
            timestamp = str(datetime.now(timezone.utc))
            audit_row = [
                timestamp,
                banned_by,
                target_name,
                target_id,
                duration,
                reason,
                thread_url,
            ]
            audit_ws.append_row(audit_row)

            # Leaderboard
            leaderboard_ws = sheet.worksheet("Leaderboard")
            col_a = leaderboard_ws.col_values(1)  # Column A (moderator names)
            try:
                row_index = col_a.index(banned_by) + 1  # 1-based index
                current_points = int(leaderboard_ws.cell(row_index, 5).value or 0)
                leaderboard_ws.update_cell(row_index, 5, current_points + 1)
                last_log_date = datetime.now(timezone.utc).strftime("%d/%m/%y")
                leaderboard_ws.update_cell(row_index, 8, last_log_date)
            except ValueError:
                # Moderator not found in leaderboard, skip
                pass

        except Exception as e:
            print(f"❌ Google Sheets error: {e}")


def run_flask() -> None:
    app.run(host="0.0.0.0", port=PORT, debug=False, use_reloader=False)


if __name__ == "__main__":
    flask_thread = threading.Thread(target=run_flask, daemon=True)
    flask_thread.start()
    bot.run(BOT_TOKEN)
