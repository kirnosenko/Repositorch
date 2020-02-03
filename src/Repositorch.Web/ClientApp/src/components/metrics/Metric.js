import React, { Fragment } from 'react';
import ContentToLoad from '../ContentToLoad';

export default function Metric(props) {
	return (
		<Fragment>
			<p><b>{props.title}</b></p>
			<ContentToLoad
				url={`api/Metrics/${props.projectMetricPath}`}
				renderData={props.renderData} />
		</Fragment>
	);
}
