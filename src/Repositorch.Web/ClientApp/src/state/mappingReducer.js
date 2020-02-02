export const mappingReducer = (state = {}, action) => {
	switch (action.type) {
		case 'addMapping':
			state[action.name] = {
				connection: action.connection
			};
			break;
		case 'updateMapping':
			state[action.name] = {
				connection: state[action.name].connection,
				done: action.done,
				total: action.total,
				error: action.error,
				working: action.working
			};
			break;
		case 'removeMapping':
			delete state[action.name];
			break;
		default:
			return state;
	}
	return state;
}
