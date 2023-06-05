
# code for generating random terrain are introduced below:
# ------------------------------------------------------------------------------
# from TerrainGeneration.RandomFormTerrain import RandomFormTerrainGenerator

# r1 = RandomFormTerrainGenerator(shape=(513, 513), roughness=0.1)
# r2 = RandomFormTerrainGenerator(shape=(513, 513), roughness=0.3)
# r3 = RandomFormTerrainGenerator(shape=(513, 513), roughness=0.5)
# r4 = RandomFormTerrainGenerator(shape=(513, 513), roughness=0.7)

# r1.generate(visual=True)
# r2.generate(visual=True)
# r3.generate(visual=True)
# r4.generate(visual=True)


# code for generating fixed form terrain are introduced below:
# ------------------------------------------------------------------------------
# from TerrainGeneration.FixedFormTerrain import FixedFormTerrainGenerator

# fftg = FixedFormTerrainGenerator(freq=12, shape=(513, 513), num_of_period=4, min_height=0, max_height=255)
# height_matrix = fftg.generate(kind='cosine', visual='3d')


# code for automation for generating designated number of raw files are introduced below:
# ------------------------------------------------------------------------------
if __name__ == '__main__':
    from Automation.TerrainGenerationAutomation import FreeFormTerrainGenerationAutomation

    fftga = FreeFormTerrainGenerationAutomation(dst='dst',
                                                num_of_file=2000,
                                                file_format="raw",
                                                shape=(513, 513),
                                                min_height_range=(10, 100),
                                                max_height_range=(101, 255),
                                                roughness_range=(0.3, 0.6),
                                                random_seed_range=(0, 20))

    fftga.start()
