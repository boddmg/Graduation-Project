#!/usr/bin/env python
import sys
from os import path, listdir
import struct
import fuel
from collections import OrderedDict
import numpy

from fuel import config
from fuel.datasets import IndexableDataset
from fuel.utils import do_not_pickle_attributes


class CAD60(object):
    def __init__(self, root_path = None, batch_size = 10):
        if not root_path:
            root_path = path.split(sys.argv[0])[0]
            root_path = path.abspath(root_path)
            self.root_path = root_path
        else:
            self.root_path = path.abspath(root_path)
        self.data_path = self.join("data.dat")
        self.labels_path = self.join("labels.dat")
        self.batch_size = batch_size
        self.shape = [0, batch_size, 170]
        pass

    def get_a_movement(self):
        root_path = self.root_path
        dirs = filter(lambda x:path.isdir(path.join(root_path,x)),listdir(root_path))

        for dir in dirs:
            current_dir = path.join(root_path, dir)
            current_file = path.join(current_dir, "activityLabel.txt")
            if not path.exists(current_file):
                continue
            index = map(lambda x:x.split(",")[:2],open(current_file, "r").readlines() )[:-1]
            for i in index:
                movement = path.join(current_dir, i[0]+".txt")
                yield movement, i

    def _get_data_shape(self):
        if self.shape[0] != 0:
            return self.shape
        col = row = batch = 0
        row = self.batch_size
        for i in self.get_a_movement():
            new_file = open(i[0]).readlines()[:-1]
            col = len(new_file[0].split(","))-2 if col == 0 else col # Remove the header and tail
            batch += len(new_file) - self.batch_size + 1
        return batch, row, col

    def join(self, src_path):
        return path.join(self.root_path,src_path)

    def get_data(self):
        if path.exists(self.data_path) and path.exists(self.labels_path):
            self.data = numpy.fromfile(self.data_path)
            self.labels = numpy.fromfile(self.labels_path)
            self.shape[0] = self.shape[0]/170/self.batch_size
            return self.data, self.labels

        shape = self._get_data_shape()
        data = numpy.zeros(shape)
        labels = numpy.zeros(shape[0], dtype=numpy.uint8)
        new_data_batch = []
        index = 0

        label_table = OrderedDict()

        for i in self.get_a_movement():
           if not label_table.has_key(i[1][1]):
               label_table[i[1][1]] = len(label_table)
           new_data_file = open(i[0]).readlines()[:-1]
           for j in range(len(new_data_file) - self.batch_size + 1):
               try:
                   new_data_batch = map(lambda x:map(float, x.split(",")[1:-1]), new_data_file[j:self.batch_size + j])
                   data[index] = new_data_batch
                   labels[index] = label_table[i[1][1]]
               except:
                   for err in new_data_batch:
                       print("error len:",len(err))
                   print("index:",index)
                   raise
                   # print(len(new_data_batch))
               finally:
                   index = index + 1
                   if index % 1000 == 0:
                       print(index)
        return data, labels



@do_not_pickle_attributes('indexables')
class CAD60Skeleton(IndexableDataset):
    provides_sources = ('features', 'targets')
    src_data = None
    src_labels = None

    def __init__(self, set_type = "train", flatten=False, **kwargs):
        self.flatten = flatten
        self.set_type = set_type

        super(CAD60Skeleton, self).__init__(OrderedDict(zip(self.provides_sources,
                                                    self._load_skeleton())),**kwargs)

    def load(self):
        self.indexables = [data[self.start:self.stop] for source, data
                           in zip(self.provides_sources, self._load_skeleton())
                           if source in self.sources]

    def _load_skeleton(self):
        if not CAD60Skeleton.src_data:
            CAD60Skeleton.src_data, CAD60Skeleton.src_labels = CAD60(path.join(config.data_path, "cad60")).get_data()
        if self.set_type == "train":
            return CAD60Skeleton.src_data[:50000], CAD60Skeleton.src_labels[:50000]
        else:
            return CAD60Skeleton.src_data[50001:], CAD60Skeleton.src_labels[50001:]


def main():
    print("start.")
    cad60_train = CAD60Skeleton("train")
    cad60_test = CAD60Skeleton("test")
    # next_batch = cad60.get_data_batch(10)
    # for i in next_batch:
    #     print(len(i[0][0]),i[1])



if __name__ == "__main__":
    main()


