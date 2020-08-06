export const mappingReducer = (state = {}, action) => {
	switch (action.type) {
		case 'updateMapping':
			state[action.name] = {
				progress: action.progress,
				errors: action.errors,
				time: action.time
			};
			break;
		default:
			return state;
	}
	return state;
}
