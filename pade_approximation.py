from scipy.interpolate import pade
from matplotlib import pyplot as plt
import numpy as np

coefs = [0,1,-1/2,1/6,-1/24,1/120,-1/720,1/5040]

p,q = pade(coefs, 4)

print(p)
print(q)

x = np.linspace(0, 1000)
plt.plot(x, 1-np.exp(-x))
plt.plot(x, p(x)/q(x), '--')
# plt.plot(x, np.poly1d(coefs[::-1])(x), lw=0.5)
plt.show()
