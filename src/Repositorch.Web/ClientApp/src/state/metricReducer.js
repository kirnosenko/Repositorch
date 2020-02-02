export const metricReducer = (state = {}, action) => {
	switch (action.type) {
		case 'setMetric':
			return {
				project: action.project,
				path: action.path
			}
		case 'clearMetric':
			return {}
		default:
			return state;
	}
}
