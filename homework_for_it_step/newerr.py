import numpy as np

arr = np.random.randint(1, 100, 10)
max_val = np.max(arr)
max_idx = np.argmax(arr)

print(arr)
print(max_val)
print(max_idx)
# 2 zhadanie
matrix = np.array([
    [1, 2, 3],
    [4, 5, 6],
    [7, 8, 9]
])

row2_sum = matrix[1, 0] + matrix[1, 1]
product_diag = np.prod(np.diag(matrix))

print(row2_sum)
print(product_diag)
