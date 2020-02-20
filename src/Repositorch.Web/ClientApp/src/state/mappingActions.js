export const addMapping = (name, connection) => {
	return {
		type: 'addMapping',
		name: name,
		connection: connection
	};
}

export const updateMapping = (name, progress, error, working) => {
	return {
		type: 'updateMapping',
		name: name,
		progress: progress,
		error: error,
		working: working
	};
}

export const removeMapping = (name) => {
	return {
		type: 'removeMapping',
		name: name
	};
}
