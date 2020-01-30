import React, { Fragment } from 'react';
import ContentToLoad from '../ContentToLoad';

export default function Metric(props) {
	return (
		<Fragment>
			<h2>{props.title}</h2>
			<ContentToLoad
				url={`api/Metrics/${props.path}`}
				renderData={props.renderData} />
		</Fragment>
	);
}
