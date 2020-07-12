import React from 'react';
import propTypes from 'prop-types';

export default function PathText(props) {
	return (
		<div className="form-group">
			<div className="heading">Path contains</div>
			<input
				type="text"
				name="path"
				value={props.settings.path}
				onChange={props.handleChange} />
		</div>
	);
}

PathText.propTypes = {
	settings: propTypes.object.isRequired,
	handleChange: propTypes.func.isRequired
};
