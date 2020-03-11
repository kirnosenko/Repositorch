import React from 'react';
import propTypes from 'prop-types';
import ContentToLoad from './ContentToLoad';

export default function ContentToLoadClosed(props) {

	const [state, setState] = React.useState(null);

	function getData() {
		return state;
	}

	async function loadData() {
		var response = await fetch(props.url);
		if (!response.ok) throw new Error(response.status);
		var json = await response.json();
		setState(json);
	}

	return (
		<ContentToLoad
			getData={getData}
			loadData={loadData}
			renderData={props.renderData}
			noloading={props.noloading} />
	);
}

ContentToLoadClosed.propTypes = {
	url: propTypes.string.isRequired,
	renderData: propTypes.func.isRequired,
	noloading: propTypes.bool,
};
