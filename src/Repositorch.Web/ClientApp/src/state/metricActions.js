export const setMetric = (project, path) => {
	return {
		type: 'setMetric',
		project: project,
		path: path
	};
}

export const clearMetric = () => {
	return {
		type: 'clearMetric'
	};
}
