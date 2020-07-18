export const updateMapping = (name, progress, error, working) => {
	return {
		type: 'updateMapping',
		name: name,
		progress: progress,
		error: error,
		working: working
	};
}
