import React, { Fragment } from 'react';
import ContentToLoadStatic from './ContentToLoadStatic';

var underfooter = {
	height: "30px",
	width: "100%"
}

var footer = {
	fontSize: 12,
	textAlign: "center",
	position: "absolute",
	left: "0",
	bottom: "0px",
	height: "30px",
	width: "100%"
}

export function Footer() {
	return (
		<Fragment>
			<div style={underfooter}>
			</div>
			<div style={footer}>
				Powered by <a href="https://github.com/kirnosenko/Repositorch"
					target="_blank" rel="noopener noreferrer">Repositorch</a>
				&nbsp;
				<ContentToLoadStatic
					url="api/Info/GetVersion"
					noloading="true"
					renderData={(version) => version} />
				&nbsp;
				(<a href="/env">ENV</a>)
			</div>
		</Fragment>
	)
}
