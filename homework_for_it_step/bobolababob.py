import pandas as pd
# я здесь сделал на английском к примеру Product. потому что захотелось 

df = pd.read_csv('transactions_data.csv')
dimensions = df.shape

rows_count = len(df)

shape_rows = df.shape[0]

col_counts = df.count()

unique_rows = df.drop_duplicates().shape[0]

most_frequent_product = df['Product'].mode()[0]

print(dimensions)
print(rows_count)
print(shape_rows)
print(col_counts)
print(unique_rows)
print(most_frequent_product)
