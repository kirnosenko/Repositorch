export const metricReducer = (state = null, action) => {
	switch (action.type) {
		case 'setMetricPath':
			return action.path;
			break;
		default:
			return state;
	}
	return state;
}
