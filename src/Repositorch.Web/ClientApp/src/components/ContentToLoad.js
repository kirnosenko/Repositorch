import React from 'react';
import Loading from './Loading';

export default function ContentToLoad(props) {

	const [state, setState] = React.useState(null);

	function getData() {
		return props.data !== undefined
			? props.data
			: state;
	}

	React.useEffect(() => {

		function setData(data) {
			props.setData !== undefined
				? props.setData(data)
				: setState(data);
		}

		async function loadData() {
			console.log(props.url);
			fetch(props.url)
				.then((response) => {
					if (!response.ok) throw new Error(response.status);
					return response.json();
				})
				.then((data) => {
					setData(data);
				});
		}

		if (getData() === null) {
			loadData();
		}
	}, [props]);

	let data = getData();
	let loading = props.noloading === undefined
		? <Loading />
		: '';

	return data === null
		? loading
		: props.renderData(data);
}
