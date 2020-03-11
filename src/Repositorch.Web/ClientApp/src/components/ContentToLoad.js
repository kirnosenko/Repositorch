import React from 'react';
import propTypes from 'prop-types';
import Loading from './Loading';

export default function ContentToLoad(props) {

	React.useEffect(() => {
		if (props.getData() === null) {
			props.loadData();
		}
	}, [props]);

	let data = props.getData();
	let loading = props.noloading === undefined
		? <Loading />
		: '';

	return data === null
		? loading
		: props.renderData(data);
}

ContentToLoad.propTypes = {
	getData: propTypes.func.isRequired,
	loadData: propTypes.func.isRequired,
	renderData: propTypes.func.isRequired,
	noloading: propTypes.bool,
};
