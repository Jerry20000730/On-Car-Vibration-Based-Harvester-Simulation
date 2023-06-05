import numpy as np
from numpy.fft import fft
import json
import zmq

# sampling rate
sp = 100
# ratio_to_reality
ratio_to_reality = 1.0 / 96.0

def ManageMessage(message):
    data = json.loads(message)
    x = []
    height = []
    for i in range(len(data['sampling_data_points_list'])):
        x.append(data['sampling_data_points_list'][i]['time'])
        height.append(data['sampling_data_points_list'][i]['height'])
    
    x = np.around(np.array(x), decimals=2)
    height = np.array(height)

    return x, height

def CalculateDiscreteFourierTransform(x, height):
    # normalization
    height = height - np.mean(height)

    output = fft(height)
    N = len(output)
    N_oneside = N // 2
    n = np.arange(N)
    T = N / sp
    freq = n / T

    freq_oneside = freq[:N_oneside]
    X_oneside = output[:N_oneside]/N_oneside

    return sum(abs(X_oneside)) * ratio_to_reality

if __name__ == '__main__':
    context = zmq.Context()
    socket = context.socket(zmq.REP)
    socket.bind("tcp://*:5555")

    while True:
        print("[INFO] Listening on port: 5555")
        #  Wait for next request from client
        message = socket.recv()
        message = message.decode()
        print("[INFO] Received request: %s" % message)
        x, height = ManageMessage(message)
        Y = CalculateDiscreteFourierTransform(x, height)
        socket.send(str(Y).encode())
        print("[INFO] Sending: " + str(Y))

    





