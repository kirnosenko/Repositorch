export const addMapping = (name, connection) => {
	return {
		type: 'addMapping',
		name: name,
		connection: connection
	};
}

export const updateMapping = (name, done, total, error, working) => {
	return {
		type: 'updateMapping',
		name: name,
		done: done,
		total: total,
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
