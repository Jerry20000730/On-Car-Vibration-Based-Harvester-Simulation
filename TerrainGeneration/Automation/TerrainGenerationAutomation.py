import math
from multiprocessing import Pool, freeze_support
from tqdm import tqdm
import itertools
import random
from typing import Tuple
import Util.TerrainUtil
from TerrainGeneration.RandomFormTerrain import RandomFormTerrainGenerator


class FreeFormTerrainGenerationAutomation:
    def __init__(self,
                 dst: str,
                 num_of_file: int = 2000,
                 file_format: str = "raw",
                 shape: Tuple[int, int] = (512, 512),
                 min_height_range: Tuple[int, int] = (10, 100),
                 max_height_range: Tuple[int, int] = (101, 255),
                 roughness_range: Tuple[float, float] = (0.3, 0.6),
                 random_seed_range: Tuple[int, int] = (0, 20)):
        
        self.dst = dst
        self.num_of_file = num_of_file
        self.file_format = file_format
        self.shape = shape
        self.min_height_range = min_height_range
        self.max_height_range = max_height_range
        self.roughness_range = roughness_range
        self.random_seed_range = random_seed_range

        assert min_height_range[1] <= max_height_range[0]

        length = self._minimum_encoding_length()
        # base-61 encoding scheme
        self.id_list = ["".join(x) for x in
                        itertools.product("abcdef0123456789ghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
                                          repeat=length)]
        self.id_list = iter(self.id_list[:self.num_of_file])

    def _minimum_encoding_length(self):
        """
        This function calculates the minimum encoding length required to encode a given number of files
        using a base-61 encoding scheme.
        
        :return: the maximum value between 4 and the length required to encode all the files in the
        object using a base-61 encoding scheme.
        """
        length = 0
        total = 0
        while total < self.num_of_file:
            length += 1
            total = math.pow(61, length)

        return max(4, length)

    def _generate(self, filename):
        """
        This function generates a random terrain using specified parameters and exports it to a file.
        
        :param filename: The name of the file to be generated and exported
        """
        min_height = random.randint(self.min_height_range[0], self.min_height_range[1])
        max_height = random.randint(self.max_height_range[0], self.max_height_range[1])
        roughness = random.uniform(self.roughness_range[0], self.roughness_range[1])
        random_seed = random.uniform(self.random_seed_range[0], self.random_seed_range[1])

        generator = RandomFormTerrainGenerator(shape=self.shape,
                                               min_height=min_height,
                                               max_height=max_height,
                                               roughness=roughness,
                                               random_seed=random_seed)
        height = generator.generate()
        Util.TerrainUtil.export(height, self.dst, filename=filename, format=self.file_format)

    def start(self):
        print("[INFO] Generation start ...")
        pbar = tqdm(total=self.num_of_file, position=0)
        update = lambda *args: pbar.update()
        pool = Pool(4)
        for filename in self.id_list:
            pool.apply_async(self._generate, args=(filename, ), callback=update)

        pool.close()
        pool.join()
        print("[INFO] Generation finished!")
