import os

ls=os.listdir()
num_lines=0

for it in ls:
    if (it[len(it)-1]=='s')and(it[len(it)-2]=='c'):
        for line in open(it):
            if len(line)>1:
                num_lines += 1

print(num_lines)
