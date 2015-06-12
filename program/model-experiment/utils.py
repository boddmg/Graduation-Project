def print_mem():
    print open("/proc/meminfo").readlines()[:2]
