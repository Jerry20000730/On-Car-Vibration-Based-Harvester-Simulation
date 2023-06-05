import numpy as np
import random
import math
import datetime
from typing import Tuple
import matplotlib.pyplot as plt
from matplotlib import cm
from matplotlib.colors import LightSource


class RandomFormTerrainGenerator:
    def __init__(self,
                 shape: Tuple[int, int],
                 roughness: float,
                 random_seed=datetime.datetime.now(),
                 min_height: int = 0,
                 max_height: int = 255):
        self.shape = shape
        self.roughness = roughness
        self.random_seed = random_seed
        self.min_height = min_height
        self.max_height = max_height

        assert self.min_height <= self.max_height

        # deal with input
        if self.roughness > 1:
            self.roughness = 1.0
        elif self.roughness < 0:
            self.roughness = 0.0

    def _get_shape_and_iterations(self, shape, max_power_of_two=13):
        """
        DESCRIPTION
        ----------
        Return the size of the terrain which is usable in
        the current algorithm

        PARAMETERS
        ----------
        :param shape: (int, int) -> the 'requesting' output shape of the terrain
        :param max_power_of_two: int -> indicates the maximum size grid that the algorithm can EVER
        attempt to make, in order to control the computational resources
        :returns: an integer of size
        """

        # deal with input
        if max_power_of_two < 3:
            max_power_of_two = 3

        for power in range(1, max_power_of_two+1):
            d = (2**power) + 1
            if max(shape) <= d:
                return (d, d), power

        # if the size is too big
        d = 2**max_power_of_two + 1
        print("Warning: Requested size was too large. Grid of size {0} returned".format(d))
        return (d, d), max_power_of_two

    def _diamond_displace(self, terrain_array, i, j, half_step, roughness):
        """
        DESCRIPTION
        ----------
        diamond step helper function

        PARAMETERS
        ----------
        :terrain_array: nd_array -> an array to record the final output
        :i int -> x-axis position
        :j int -> y-axis position
        :half_step int -> half of the increments
        :roughness: float -> a value between 0 and 1
        indicating the roughness of the landscape.

        :returns value of the filling

        """
        upper_left = terrain_array[i - half_step, j - half_step]
        upper_right = terrain_array[i - half_step, j + half_step]
        lower_left = terrain_array[i + half_step, j - half_step]
        lower_right = terrain_array[i + half_step, j - half_step]

        average = (upper_left + upper_right + lower_left + lower_right) / 4.0
        rand_roughness = random.uniform(0, 1)

        return (roughness * rand_roughness) + (1.0 - roughness) * average

    def _diamond_step(self, terrain_array, step_size, roughness):
        """
        DESCRIPTION
        ----------
        Do a diamond step for one iteration
        V1             unknown                 V3
        unknown       ->Avg(V1, V2, V3, V4)    unknown
        V2             unknown                 V4

        PARAMETERS
        ----------
        :terrain_array: nd_array -> an array to record the final output
        :step_size: int -> increments along two axes
        :roughness: float -> a value between 0 and 1
        indicating the roughness of the landscape.

        """
        half_step = math.floor(step_size / 2)
        x_steps = range(half_step, terrain_array.shape[0], step_size)
        y_steps = x_steps[:]

        for i in x_steps:
            for j in y_steps:
                if terrain_array[i, j] == -1:
                    terrain_array[i, j] = self._diamond_displace(terrain_array, i, j, half_step, roughness)

    def _square_displace(self, terrain_array, i, j, half_step, roughness):
        """
        DESCRIPTION
        ----------
        square step helper function

        PARAMETERS
        ----------
        :terrain_array: nd_array -> an array to record the final output
        :i int -> x-axis position
        :j int -> y-axis position
        :half_step int -> half of the increments
        :roughness: float -> a value between 0 and 1
        indicating the roughness of the landscape.

        :returns value of the filling

        """

        _sum = 0.0
        divide_by = 4

        if i - half_step >= 0:
            _sum += terrain_array[i - half_step, j]
        else:
            divide_by -= 1
        if i + half_step < terrain_array.shape[0]:
            _sum += terrain_array[i + half_step, j]
        else:
            divide_by -= 1
        if j - half_step >= 0:
            _sum += terrain_array[i, j - half_step]
        else:
            divide_by -= 1
        if j + half_step < terrain_array.shape[0]:
            _sum += terrain_array[i, j + half_step]
        else:
            divide_by -= 1

        average = _sum / divide_by
        rand_roughness = random.uniform(0, 1)

        return (roughness * rand_roughness) + (1.0 - roughness) * average

    def _square_step(self, terrain_array, step_size, roughness):
        """
        DESCRIPTION
        ----------
        Do a square step for one iteration (usually after diamond)
        V1              ->Avg(V1, V3)            V3
        ->Avg(V1, V2)     V5                     ->Avg(V3, V4)
        V2              ->Avg(V2, V4)            V4

        PARAMETERS
        ----------
        :terrain_array: nd_array -> an array to record the final output
        :step_size: int -> increments along two axes
        :roughness: float -> a value between 0 and 1
        indicating the roughness of the landscape.

        """

        half_step = math.floor(step_size / 2)
        # horizontal step
        steps_x_horiz = range(0, terrain_array.shape[0], step_size)
        steps_y_horiz = range(half_step, terrain_array.shape[1], step_size)
        # vertical step
        steps_x_verts = range(half_step, terrain_array.shape[0], step_size)
        steps_y_verts = range(0, terrain_array.shape[1], step_size)

        for i in steps_x_horiz:
            for j in steps_y_horiz:
                terrain_array[i, j] = self._square_displace(terrain_array, i, j, half_step, roughness)

        for i in steps_x_verts:
            for j in steps_y_verts:
                terrain_array[i, j] = self._square_displace(terrain_array, i, j, half_step, roughness)

    def _visualize(self, height):
        """
        DESCRIPTION
        ----------
        This function visualizes a 3D plot of a given height map using matplotlib.
        
        :param height: a 2D numpy array representing the height map of a terrain. The values in the
        array represent the height of each point in the terrain
        """
        # x axis
        x = np.arange(0, height.shape[0], 1)
        y = np.arange(0, height.shape[1], 1)
        x, y = np.meshgrid(x, y)
        z = height

        # Set up plot
        fig, ax = plt.subplots(figsize=(10, 10), subplot_kw=dict(projection='3d'))

        ls = LightSource(270, 45)
        # To use a custom hillshading mode, override the built-in shading and pass
        # in the rgb colors of the shaded surface calculated from "shade".
        rgb = ls.shade(z, cmap=cm.gist_earth, vert_exag=0.1, blend_mode='soft')
        surf = ax.plot_surface(x, y, z, rstride=1, cstride=1, facecolors=rgb,
                               linewidth=0, antialiased=False, shade=False)
        ax.set_zlim(self.min_height, self.max_height * 5)

        plt.show()

    def generate(self, visual=False):
        """
        DESCRIPTION
        ----------
        Generate random-form terrain based on Square-Diamond Algorithm

        :returns heightmap array
        """
        # verify if the shape can be expressed
        # as the power of 2
        working_shape, iterations = self._get_shape_and_iterations(self.shape)
        # create the array
        terrain = np.full(working_shape, -1, dtype=np.float32)
        # create the array for final terrain
        final_terrain = np.zeros(working_shape, dtype=np.uint8)
        # seed the random number generator
        random.seed(self.random_seed)

        # decide the corner value
        terrain[0, 0] = terrain[working_shape[0] - 1, 0] = terrain[0, working_shape[0] - 1] = terrain[
            working_shape[0] - 1, working_shape[0] - 1] = random.uniform(0, 1)

        # algorithm starts
        for i in range(iterations):
            r = math.pow(self.roughness, i)
            step_size = math.floor((working_shape[0] - 1) / math.pow(2, i))
            self._diamond_step(terrain, step_size, r)
            self._square_step(terrain, step_size, r)

        for i in range(working_shape[0]):
            for j in range(working_shape[1]):
                final_terrain[i, j] = int(self.min_height + terrain[i, j] * (self.max_height - self.min_height))

        if visual:
            self._visualize(final_terrain)

        return final_terrain


