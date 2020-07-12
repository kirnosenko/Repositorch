import React from 'react';
import propTypes from 'prop-types';

export default function AuthorSelector(props) {
	return (
		<div className="form-group">
			<div className="heading">Author</div>
			<select
				className="custom-select"
				name="author"
				value={props.settings.author ?? ""}
				onChange={props.handleChange} >
				<option value="">Not selected</option>
				{props.settings.authors.map(author => {
					return <option key={author} value={author}>{author}</option>
				})}
			</select>
		</div>
	);
}

AuthorSelector.propTypes = {
	settings: propTypes.object.isRequired,
	handleChange: propTypes.func.isRequired
};
