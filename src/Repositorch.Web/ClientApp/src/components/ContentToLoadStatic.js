import React from 'react';
import propTypes from 'prop-types';
import ContentToLoad from './ContentToLoad';

export default function ContentToLoadStatic(props) {

	const [data, setData] = React.useState(null);

	async function loadData() {
		var response = await fetch(props.url);
		if (!response.ok) throw new Error(response.status);
		var json = await response.json();
		setData(json);
	}

	return (
		<ContentToLoad
			getData={() => data}
			loadData={loadData}
			renderData={props.renderData}
			noloading={props.noloading} />
	);
}

ContentToLoadStatic.propTypes = {
	url: propTypes.string.isRequired,
	renderData: propTypes.func.isRequired,
	noloading: propTypes.bool,
};
