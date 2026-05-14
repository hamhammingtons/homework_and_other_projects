local Players = game:GetService("Players")
local HttpService = game:GetService("HttpService")
local DataStoreService = game:GetService("DataStoreService")
local BanDataStore = DataStoreService:GetDataStore("BanSystem_v2")

-- Configuration
local NGROK_URL = "" -- Wont send my ngrok_url here because i want to keep it private 

-- List of allowed moderators (UserIds) - add more as needed
local ALLOWED_MODERATORS = {
	[3471798004] = true, -- Replace with actual moderator UserIds
}

-- Minimum rank required to use ban command (set to 0 to disable rank check)
local MINIMUM_RANK = 255 -- Adjust based on your rank system

-- Cache for bans
local banCache = {}

-- Check if player has permission to use ban command
local function hasPermission(player)
	-- Check if player is in allowed moderators list
	if ALLOWED_MODERATORS[player.UserId] then
		return true
	end
	
	-- Check if player has a rank system (adjust based on your game)
	-- Example: Check for leaderstats rank
	local leaderstats = player:FindFirstChild("leaderstats")
	if leaderstats then
		local rank = leaderstats:FindFirstChild("Rank")
		if rank and rank:IsA("IntValue") and rank.Value >= MINIMUM_RANK then
			return true
		end
	end
	
	-- Check if player is game owner
	if player.UserId == game.CreatorId then
		return true
	end
	
	return false
end

-- Helper function to parse duration string (e.g., "3d", "2m", "1y")
local function parseDuration(durationStr)
	local number = tonumber(string.match(durationStr, "%d+"))
	local unit = string.match(durationStr, "%a+")
	
	if not number or not unit then
		return nil
	end
	
	unit = string.lower(unit)
	local seconds = 0
	
	if unit == "s" then
		seconds = number
	elseif unit == "m" then
		seconds = number * 60
	elseif unit == "h" then
		seconds = number * 3600
	elseif unit == "d" then
		seconds = number * 86400
	elseif unit == "w" then
		seconds = number * 604800
	elseif unit == "mo" then
		seconds = number * 2592000 -- 30 days
	elseif unit == "y" then
		seconds = number * 31536000
	else
		return nil
	end
	
	return seconds
end

-- Get ban info for a player
local function getBanInfo(userId)
	if banCache[userId] then
		return banCache[userId]
	end
	
	local success, result = pcall(function()
		return BanDataStore:GetAsync("ban_" .. userId)
	end)
	
	if success and result then
		banCache[userId] = result
		return result
	end
	
	return nil
end

-- Save ban info for a player
local function saveBan(userId, banData)
	banCache[userId] = banData
	
	local success, err = pcall(function()
		BanDataStore:SetAsync("ban_" .. userId, banData)
	end)
	
	return success
end

-- Remove ban for a player
local function removeBan(userId)
	banCache[userId] = nil
	
	pcall(function()
		BanDataStore:RemoveAsync("ban_" .. userId)
	end)
end

-- Check if a ban is expired
local function isBanExpired(banInfo)
	if not banInfo then return true end
	if banInfo.durationSeconds == 0 then return false end -- Permanent ban
	
	return os.time() > banInfo.bannedAt + banInfo.durationSeconds
end

-- Send ban data to ngrok Flask backend
local function sendToBackend(targetName, targetId, reason, duration, bannedBy)
	local payload = {
		targetName = targetName,
		targetId = targetId,
		reason = reason,
		duration = duration,
		bannedBy = bannedBy
	}
	
	local jsonData = HttpService:JSONEncode(payload)
	
	local success, result = pcall(function()
		return HttpService:PostAsync(NGROK_URL, jsonData, Enum.HttpContentType.ApplicationJson)
	end)
	
	if success then
		print("Successfully sent ban data to backend")
		return true
	else
		print("Failed to send ban data to backend: " .. tostring(result))
		return false
	end
end

-- Ban a player
local function banPlayer(targetUserId, targetUsername, durationStr, reason, bannedBy)
	-- Check if user is already banned
	local existingBan = getBanInfo(targetUserId)
	local isAlreadyBanned = existingBan and not isBanExpired(existingBan)
	
	local durationSeconds = 0 -- Default to permanent
	
	if durationStr and durationStr ~= "" then
		local parsedDuration = parseDuration(durationStr)
		if parsedDuration then
			durationSeconds = parsedDuration
		else
			return false, "Invalid duration format. Use format like 3d, 3m, 3y"
		end
	end
	
	local banData = {
		userId = targetUserId,
		username = targetUsername,
		reason = reason or "No reason provided",
		durationSeconds = durationSeconds,
		durationRaw = durationStr,
		bannedAt = os.time(),
		bannedBy = bannedBy
	}
	
	local success = saveBan(targetUserId, banData)
	if not success then
		return false, "Failed to save ban to DataStore"
	end
	
	-- Only send to backend if this is a new ban or previous ban expired
	if isAlreadyBanned then
		print("User is already banned, log skipped.")
	else
		sendToBackend(targetUsername, targetUserId, reason or "No reason provided", durationStr, bannedBy)
	end
	
	-- Kick the player if they're in the game
	local targetPlayer = Players:GetPlayerByUserId(targetUserId)
	if targetPlayer then
		local kickMessage = "You have been banned!"
		if reason and reason ~= "" then
			kickMessage = kickMessage .. "\nReason: " .. reason
		end
		if durationSeconds > 0 then
			kickMessage = kickMessage .. "\nExpires: " .. os.date("%Y-%m-%d %H:%M:%S", banData.bannedAt + durationSeconds)
		else
			kickMessage = kickMessage .. "\nThis is a permanent ban."
		end
		targetPlayer:Kick(kickMessage)
	end
	
	-- Return with info about whether log was skipped
	return true, banData, isAlreadyBanned
end

-- Handle chat commands
local function onPlayerChatted(player, message)
	-- Check if message starts with :ban
	if string.sub(message, 1, 4):lower() ~= ":ban" then return end
	
	-- Check permissions
	if not hasPermission(player) then
		print(player.Name .. " attempted to use :ban but lacks permission")
		return
	end
	
	-- Parse the command: :ban [targetName] [duration] [reason]
	local args = {}
	for arg in string.gmatch(string.sub(message, 6), "[^%s]+") do
		table.insert(args, arg)
	end
	
	if #args < 1 then
		print("Usage: :ban [username/userId] [duration] [reason]")
		return
	end
	
	local targetName = args[1]
	local duration = args[2] or ""
	local reason = table.concat(args, " ", 3) or "No reason provided"
	
	-- If duration looks like a reason (no number), treat it as part of reason
	if duration and not string.match(duration, "%d") then
		reason = #args >= 2 and table.concat(args, " ", 2) or "No reason provided"
		duration = ""
	end
	
	-- Find the target player
	local targetUserId = nil
	local targetUsername = nil
	
	-- Try to find by username first (exact match)
	local targetPlayer = Players:FindFirstChild(targetName)
	if targetPlayer and targetPlayer:IsA("Player") then
		targetUserId = targetPlayer.UserId
		targetUsername = targetPlayer.Name
	else
		-- Try to find by UserId
		local userIdNum = tonumber(targetName)
		if userIdNum then
			targetUserId = userIdNum
			targetPlayer = Players:GetPlayerByUserId(userIdNum)
			if targetPlayer then
				targetUsername = targetPlayer.Name
			else
				targetUsername = "UserId:" .. userIdNum
			end
		else
			-- Try partial name match for players in game
			for _, p in ipairs(Players:GetPlayers()) do
				if string.find(string.lower(p.Name), string.lower(targetName)) then
					targetUserId = p.UserId
					targetUsername = p.Name
					break
				end
			end
			
			-- If still not found, try to get UserId from username (for offline players)
			if not targetUserId then
				local success, userIdResult = pcall(function()
					return Players:GetUserIdFromNameAsync(targetName)
				end)
				if success and userIdResult then
					targetUserId = userIdResult
					targetUsername = targetName
				end
			end
		end
	end
	
	if not targetUserId then
		print("Player not found: " .. targetName)
		return
	end
	
	-- Execute the ban
	local success, result, wasAlreadyBanned = banPlayer(targetUserId, targetUsername, duration, reason, player.Name)
	
	if success then
		print("=== BAN INFO ===")
		print("Target: " .. targetUsername .. " (" .. targetUserId .. ")")
		print("Reason: " .. (reason or "No reason provided"))
		print("Duration: " .. (duration ~= "" and duration or "permanent"))
		print("Banned by: " .. player.Name)
		if wasAlreadyBanned then
			print("Status: User was already banned, log skipped")
		end
		print("================")
		print("user banned: " .. targetUsername .. ", check output for more data")
	else
		print("Failed to ban: " .. (result or "Unknown error"))
	end
end

-- Check if a player is banned when they join
local function onPlayerAdded(player)
	local banInfo = getBanInfo(player.UserId)
	
	if banInfo then
		if isBanExpired(banInfo) then
			removeBan(player.UserId)
			print("Ban expired for: " .. player.Name)
		else
			local kickMessage = "You are banned!"
			if banInfo.reason then
				kickMessage = kickMessage .. "\nReason: " .. banInfo.reason
			end
			if banInfo.durationSeconds > 0 then
				local expiresAt = banInfo.bannedAt + banInfo.durationSeconds
				kickMessage = kickMessage .. "\nExpires: " .. os.date("%Y-%m-%d %H:%M:%S", expiresAt)
			else
				kickMessage = kickMessage .. "\nThis is a permanent ban."
			end
			kickMessage = kickMessage .. "\nBanned by: " .. (banInfo.bannedBy or "Unknown")
			
			player:Kick(kickMessage)
			print("Kicked banned player: " .. player.Name)
		end
	end
	
	player.Chatted:Connect(function(message)
		onPlayerChatted(player, message)
	end)
end

-- Connect events
Players.PlayerAdded:Connect(onPlayerAdded)

for _, player in ipairs(Players:GetPlayers()) do
	task.spawn(function()
		onPlayerAdded(player)
	end)
end

print("Ban System loaded successfully!")
print("Usage: :ban [username/userId] [duration] [reason]")
print("Duration examples: 3d, 3m, 3h, 3w, 3mo, 3y")
print("Leave duration empty for permanent ban")
