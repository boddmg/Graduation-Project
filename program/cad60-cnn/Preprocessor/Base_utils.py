#!/usr/bin/env python
__author__ = 'boddmg'


import numpy
import time
import hickle

# src_data format is [batch number, batch size, sample size]

class Preprocessor(object):
    def __init__(self):
        pass

    def run(self, src_data, src_labels):
        pass

class PreprocessorList(Preprocessor):
    def __init__(self, list):
        self.list = list
        pass

    def run(self, src_data = None, src_labels = None):
        for i in self.list:
            src_data, src_labels = i.run(src_data, src_labels)
        return src_data, src_labels

class DataLoad(Preprocessor):
    def __init__(self, path):
        self.path = path

    def run(self, src_data = None, src_labels = None):
        data = hickle.load(self.path)
        return data["data"], data["labels"]

class DataDump(Preprocessor):
    def __init__(self, path = time.strftime('%y-%m-%d-%H-%M-%S.hkl')):
        self.path = path

    def run(self, src_data, src_labels):
        hickle.dump({
            "data":src_data,
            "labels":src_labels
        },self.path,"w")
        return src_data,src_labels

class Shuffle(Preprocessor):
    def __init__(self):
        pass

    def run(self, src_data, src_labels):
        indices = numpy.random.permutation(src_data.shape[0])
        src_data = src_data[indices]
        src_labels = src_labels[indices]
        return src_data, src_labels

class Index(Preprocessor):
    def __init__(self, index = None):
        self.index = index

    def run(self, src_data, src_labels):
        if self.index:
            dst_data = numpy.zeros([src_data.shape[0], src_data.shape[1], len(self.index)],  dtype=numpy.float32)
            for i in range(len(src_data)):
                for j in range(len(src_data[i])):
                    dst_data[i][j] = src_data[i][j][self.index]
            return dst_data, src_labels
        return src_data, src_labels

class PreFlattener(Preprocessor):
    def __init__(self):
        pass

    def run(self, src_data, src_labels):
        src_data = src_data.reshape(src_data.shape[0], 1, src_data.shape[1] * src_data.shape[2])
        return src_data, src_labels

class SplitIntoBatches(Preprocessor):
    def __init__(self, batch_size = 1, stride = 1):
        self.batch_size = batch_size
        self.stride = stride

    def getSplitPoint(self, src_labels):
        label_split_point = [0]
        last_label = src_labels[0]
        for i in range(len(src_labels)):
            if src_labels[i] != last_label:
                last_label = src_labels[i]
                label_split_point.append(i)
        return label_split_point

    def run(self, src_data, src_labels):
        data_point_size = src_data.shape[2]
        split_point = self.getSplitPoint(src_labels)
        batch_index = []

        for i in range(len(split_point) - 1):
            start_point = split_point[i]
            end_point = split_point[i+1]
            for j in range(start_point, end_point, self.stride):
                if j+self.batch_size <= end_point:
                    batch_index.append([j,j+self.batch_size])
        dst_data = numpy.zeros([len(batch_index), self.batch_size,data_point_size],  dtype=numpy.float32)
        dst_labels = numpy.zeros([len(batch_index)], dtype=numpy.uint8)

        index = 0
        for i in batch_index:
            dst_data[index] = src_data[i[0]:i[1]].reshape(self.batch_size, data_point_size)
            dst_labels[index] = src_labels[i[0]]
            index += 1
        return dst_data, dst_labels


class Monitor(Preprocessor):
    def __init__(self):
        pass

    def run(self,src_data, src_labels):

        print("Monitor:",src_data.shape, src_labels.shape)
        return src_data, src_labels


def main():
    src_data, src_labels = PreprocessorList([DataLoad("default.hkl"),DataDump("default1.hkl")]).run()
    print src_data.shape, src_labels.shape

if __name__ == '__main__':
    main()
