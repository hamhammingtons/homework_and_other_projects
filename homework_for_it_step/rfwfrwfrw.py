import pandas as pd
df_ratings = pd.read_csv('ratings.csv')
fours_count = df_ratings[df_ratings['rating'] == 4.0].groupby('userId')['rating'].count()
max_fours = fours_count.max()
print(max_fours)


df_shares = pd.read_csv('movies_share.csv')
sum_share_9 = df_shares[df_shares['movieId'] == 9]['share'].sum()
print(sum_share_9)

def classify_rating(rating):
    if rating <= 2:
        return 'bad'
    elif rating <= 4:
        return 'good'
    elif rating >= 4.5:
        return 'very good'

df_ratings['class'] = df_ratings['rating'].apply(classify_rating)
print(df_ratings.head())
