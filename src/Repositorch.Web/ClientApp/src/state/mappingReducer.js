export const mappingReducer = (state = {}, action) => {
	switch (action.type) {
		case 'updateMapping':
			state[action.name] = {
				progress: action.progress,
				errors: action.errors,
				time: action.time
			};
			return Object.assign({}, state)
		default:
			return state;
	}
	return state;
}
