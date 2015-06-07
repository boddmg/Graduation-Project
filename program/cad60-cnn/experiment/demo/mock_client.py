#!/usr/bin/env python
_author__ = 'boddmg'

def main():
    import zmq
    import uuid
    data = range(144)
    context = zmq.Context()
    socket = context.socket(zmq.REQ)
    socket.connect("tcp://localhost:5555")
    client_id = uuid.uuid1().bytes
    for i in range(48):
        socket.send_pyobj({client_id:data})
        if socket.recv() != "ok!":
            raise

if __name__ == '__main__':
    # import timeit
    # print timeit.timeit("main()",number=1)
    import time
    s = time.clock()
    main()
    print time.clock()-s

