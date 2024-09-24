class_name MathUtil

static var RNG: RandomNumberGenerator:
	get:
		if RNG == null:
			RNG = RandomNumberGenerator.new()
			
		RNG.randomize()
		
		return RNG