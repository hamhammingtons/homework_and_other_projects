df = pd.read_csv('efficiency_data.csv')
df_sorted = df.sort_values(by='efficiency', ascending=False)

df_sorted['efficiency'].plot(kind='bar')
plt.title('Efficiency Distribution')
plt.show()

# 2. Visualize distribution by groups
df.groupby('group')['efficiency'].mean().plot(kind='pie', autopct='%1.1f%%')
plt.title('Efficiency by Group')
plt.show()

  # здесь нужно import rfwfrwfrw.py (рекомендую)
