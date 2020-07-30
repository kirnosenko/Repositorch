export const mappingReducer = (state = {}, action) => {
	switch (action.type) {
		case 'updateMapping':
			state[action.name] = {
				progress: action.progress,
				errors: action.errors,
				working: action.working
			};
			break;
		default:
			return state;
	}
	return state;
}
