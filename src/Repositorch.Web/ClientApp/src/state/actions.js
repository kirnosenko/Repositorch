export const addMapping = (name) => {
	return {
		type: 'addMapping',
		name: name
	};
}

export const removeMapping = (name) => {
	return {
		type: 'removeMapping',
		name: name
	};
}
