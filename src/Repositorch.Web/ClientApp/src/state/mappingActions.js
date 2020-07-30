export const updateMapping = (name, progress, errors, working) => {
	return {
		type: 'updateMapping',
		name: name,
		progress: progress,
		errors: errors,
		working: working
	};
}
