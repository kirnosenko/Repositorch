export const updateMapping = (name, progress, errors, time) => {
	return {
		type: 'updateMapping',
		name: name,
		progress: progress,
		errors: errors,
		time: time
	};
}
