#!/usr/bin/env python
import sys
from os import path, listdir
import struct
from collections import OrderedDict
import numpy



class CAD60(object):
    data_info = {
        "index_range":{
            "train":[0,2],
            "test":[3,3]
        },
        "path":{
            "train":{
                "data":"batch_%s_train_data.dat",
                "labels":"batch_%s_train_labels.dat"
            },
            "test":{
                "data":"batch_%s_test_data.dat",
                "labels":"batch_%s_test_labels.dat"
        }}}

    def __init__(self, root_path = None, batch_size = 10, data_type = "train"):
        print(data_type)
        if not root_path:
            import fuel
            root_path = path.abspath(path.join(fuel.config.data_path,"cad60") )
            self.root_path = root_path
        else:
            self.root_path = path.abspath(root_path)

        self.data_type = data_type
        self.batch_size = batch_size
        self.shape = [0, batch_size, 170]
        self.data_path = self.join(self.data_info["path"][data_type]["data"] % (str(batch_size)))
        self.labels_path = self.join(self.data_info["path"][data_type]["labels"] % (str(batch_size)))
        pass

    def get_a_movement(self):
        root_path = self.root_path
        dirs = filter(lambda x:path.isdir(path.join(root_path,x)),listdir(root_path))
        person_index = 0

        for dir in dirs:
            if  person_index >= self.data_info["index_range"][self.data_type][0] and \
                person_index <= self.data_info["index_range"][self.data_type][1]:

                current_dir = path.join(root_path, dir)
                current_file = path.join(current_dir, "activityLabel.txt")
                if not path.exists(current_file):
                    continue
                index = map(lambda x:x.split(",")[:2],open(current_file, "r").readlines() )[:-1]
                for i in index:
                    file_path = path.join(current_dir, i[0]+".txt")
 #                   if i[1]=="random":
 #                       continue
                    yield file_path, i, person_index
            person_index += 1

    def _get_data_shape(self):
        if self.shape[0] != 0:
            return self.shape
        col = row = batch = 0
        row = self.batch_size
        for file_path, i, person_index  in self.get_a_movement():
            new_file = open(file_path).readlines()[:-1]
            col = len(new_file[0].split(","))-2 if col == 0 else col # Remove the header and tail
            batch += len(new_file) - self.batch_size + 1
        self.shape[0] = batch
        self.shape[1] = row
        self.shape[2] = col
        return self.shape


    def join(self, src_path):
        return path.join(self.root_path,src_path)

    def get_data(self):
        if path.exists(self.data_path):
            self.data = numpy.fromfile(self.data_path, dtype=numpy.float32)
            self.labels = numpy.fromfile(self.labels_path, dtype = numpy.uint8)
            self.shape[0] = self.data.shape[0]/170/self.batch_size
            self.data = self.data.reshape(self.shape)
            label_table = OrderedDict()

            for i in self.get_a_movement():
                if not label_table.has_key(i[1][1]):
                    label_table[i[1][1]] = len(label_table)

            return self.data, self.labels, label_table

        shape = self._get_data_shape()
        print "shape:",shape
        data = numpy.zeros(shape, dtype=numpy.float32)
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
               finally:
                   index = index + 1
                   if index % 1000 == 0:
                       print(index)
        data.tofile(self.data_path)
        labels.tofile(self.labels_path)
        return data, labels, label_table


