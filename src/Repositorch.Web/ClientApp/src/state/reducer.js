export const reducer = (state = { mappings: [] }, action) => {
    switch (action.type) {
        case 'addMapping':
            return Object.assign({}, state, {
                mappings: state.mappings.concat(action.name)
            });
        case 'removeMapping':
            return Object.assign({}, state, {
                mappings: state.mappings.filter(x => x !== action.name)
            });
        default:
            return state;
    }
}
