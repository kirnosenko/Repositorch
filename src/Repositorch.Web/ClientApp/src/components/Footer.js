import React, { Fragment } from 'react';

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
				Powered by <a href="https://github.com/kirnosenko/Repositorch">Repositorch</a> 0.1.0 alpha
			</div>
		</Fragment>
	)
}
