import React from 'react';
import propTypes from 'prop-types';

export default function SettingsForm(props) {

	const [settings, setSettings] = React.useState(props.settings);

	function setSetting(name, value) {
		setSettings({
			...settings,
			[name]: value
		});
	}

	function handleChange(evt) {
		const key = evt.target.name;
		const value = evt.target.type === "checkbox"
			? evt.target.checked
			: evt.target.value;

		setSetting(key, value);
	}

	async function handleSubmit(event) {
		event.preventDefault();
		props.updateSettings(settings);
	}

	return (
		<form onSubmit={handleSubmit}>
			{props.getFormContent(settings, setSetting, handleChange)}
			<button
				type="submit"
				className="btn btn-outline-dark btn-sm">
				Update...
			</button>
		</form>
	);
}

SettingsForm.propTypes = {
	settings: propTypes.object.isRequired,
	updateSettings: propTypes.func.isRequired,
	getFormContent: propTypes.func.isRequired
};
