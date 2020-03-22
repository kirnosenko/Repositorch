import React, { Fragment } from 'react';

export const metricLayout = (title, time, body) => {

	if (time === null) {
		time = 'from cache';
	}
	else if (time === undefined) {
		time = 'unknown';
	}

	return (
		<Fragment>
			<p>
				<b>{title}</b>
				&nbsp;
				<small>(generation time: {time})</small>
			</p>
			{body}
		</Fragment>
	);
}
