export const reducer = (state = { mappings: {} }, action) => {
    var data = state.mappings;
    switch (action.type) {
        case 'addMapping':
            data[action.name] = {
                connection: action.connection
            };
            break;
        case 'updateMapping':
            data[action.name] = {
                connection: data[action.name].connection,
                done: action.done,
                total: action.total,
                error: action.error,
                working: action.working
            };
            break;
        case 'removeMapping':
            delete data[action.name];
            break;
        default:
            return state;
    }
    return { mappings: data };
}
