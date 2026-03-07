import matplotlib.pyplot as plt
import numpy as np

months = ['July', 'Aug', 'Sept', 'Oct', 'Nov', 'Dec']
sales_2016 = [100, 120, 110, 130, 150, 170]
sales_2017 = [120, 140, 130, 150, 180, 210]
sales_2018 = [140, 160, 150, 180, 220, 260]

plt.plot(months, sales_2016, label='2016')
plt.plot(months, sales_2017, label='2017')
plt.plot(months, sales_2018, label='2018')
plt.legend()
plt.savefig('plot_1.png')
plt.close()

plt.plot(months, sales_2016, color='magenta', linestyle='-.', label='2016')
plt.plot(months, sales_2017, color='black', linestyle='--', label='2017')
plt.plot(months, sales_2018, color='teal', label='2018')
plt.legend()
plt.savefig('plot_2.png')
plt.close()