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
        self.batch_size = batch_size
        pass

    def get_target(self):
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

    def get_data_shape(self):
        col = row = batch = 0
        row = self.batch_size
        for i in self.get_target():
            new_file = open(i[0]).readlines()[:-1]
            col = len(new_file[0].split(","))-2 if col == 0 else col # Remove the header and tail
            batch += len(new_file) - self.batch_size + 1
        return batch, row, col

    def join(self, src_path):
        return path.join(self.root_path,src_path)

    def get_data(self):
        shape = self.get_data_shape()
        data = numpy.zeros(shape)
        labels = numpy.zeros(shape[0], dtype=numpy.uint8)
        new_data_batch = []
        index = 0

        label_table = OrderedDict()
        if path.exists(self.join("data.dat")) and path.exists(self.join("labels.dat")):
            return numpy.fromfile("data.dat").reshape(shape), numpy.fromfile("labels.dat")
        for i in self.get_target():
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

    def __init__(self, flatten=False, **kwargs):
        self.flatten = flatten

        super(CAD60Skeleton, self).__init__(OrderedDict(zip(self.provides_sources,
                                                    self._load_skeleton())),**kwargs)

    def load(self):
        self.indexables = [data[self.start:self.stop] for source, data
                           in zip(self.provides_sources, self._load_mnist())
                           if source in self.sources]

def main():
    print("start.")
    cad60 = CAD60()
    # next_batch = cad60.get_data_batch(10)
    # for i in next_batch:
    #     print(len(i[0][0]),i[1])
    return cad60.get_data()



if __name__ == "__main__":
    main()


