import matplotlib.pyplot as plt

months = ['July', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec']
sales_2016 = [150, 160, 145, 180, 210, 250]
sales_2017 = [170, 185, 160, 210, 230, 290]
sales_2018 = [200, 220, 190, 240, 270, 320]

def get_top_three(m, s):
    combined = sorted(zip(m, s), key=lambda x: x[1], reverse=True)
    return zip(*combined[:3])

top_m_2016, top_s_2016 = get_top_three(months, sales_2016)
top_m_2017, top_s_2017 = get_top_three(months, sales_2017)
top_m_2018, top_s_2018 = get_top_three(months, sales_2018)

light_green_hex = '#90EE90'
light_red_hex = '#FFCCCB'

fig, axs = plt.subplots(1, 3, figsize=(15, 5))

axs[0].bar(top_m_2016, top_s_2016, color=light_red_hex)
axs[0].set_title('Top 3 - 2016', color=light_green_hex)

axs[1].bar(top_m_2017, top_s_2017, color=light_red_hex)
axs[1].set_title('Top 3 - 2017', color=light_green_hex)

axs[2].bar(top_m_2018, top_s_2018, color=light_red_hex)
axs[2].set_title('Top 3 - 2018', color=light_green_hex)

plt.tight_layout()
plt.show()