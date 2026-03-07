import pandas as pd

df_transactions = pd.read_csv('transactions.csv')
task_1_result = df_transactions[df_transactions['Product'] == '_8'].sort_values('Cost').iloc[0]['ID']
print(task_1_result)

df_power = pd.read_csv('power.csv')

baltic_countries = ['Latvia', 'Lithuania', 'Estonia']
target_categories = [4, 12, 21]

filtered_df = df_power[
    (df_power['country'].isin(baltic_countries)) &
    (df_power['year'] >= 2005) &
    (df_power['year'] <= 2010) &
    (df_power['quantity'] > 0) &
    (df_power['category'].isin(target_categories))
]

task_2_result = filtered_df['quantity'].sum()
print(task_2_result)
