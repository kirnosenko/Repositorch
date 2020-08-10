import React, { Fragment } from 'react'
import ContentToLoadStatic from './ContentToLoadStatic'

export default function Environment() {

	function renderVariables(variables) {
		return (
			Object.keys(variables).map(key => {
				return (
					<Fragment>
						<br /><b>{`${key}: `}</b>{`${variables[key]}`}
					</Fragment>
				)
			})
		);
	}

	return (
		<ContentToLoadStatic
			url="api/Info/GetVariables"
			renderData={renderVariables} />
	);
}
