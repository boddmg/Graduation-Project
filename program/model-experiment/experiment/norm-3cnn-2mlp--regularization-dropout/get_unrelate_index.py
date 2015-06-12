__author__ = 'boddmg'

import hickle

def main():
    data = hickle.load("./cad60_train_feature.hkl")
    print len(data['data'])
    singal_data = data['data'][0][0]
    index = []
    for i in range(len(singal_data)):
        if 1.0 == singal_data[i] or 0.0 == singal_data[i]:
            index.append(i)
    print index
    print singal_data[index]
    print len(data['labels'])

if __name__ == '__main__':
    main()
