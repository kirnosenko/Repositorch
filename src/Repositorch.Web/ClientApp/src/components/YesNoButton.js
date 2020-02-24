import React, { Fragment, useState } from 'react';
import { Modal, ModalHeader, ModalBody, ModalFooter } from 'reactstrap';

export const YesNoButton = (props) => {

	const [modal, setModal] = useState(false);
	const toggle = () => setModal(!modal);
	const action = () => {
		props.yesAction();
		toggle();
	}

	return (
		<Fragment>
			<button
				type="button"
				className="btn btn-outline-dark btn-sm"
				onClick={toggle}>{props.label}</button>
			<Modal isOpen={modal} toggle={toggle}>
				<ModalHeader toggle={toggle}>{props.title}</ModalHeader>
				<ModalBody>{props.text}</ModalBody>
				<ModalFooter>
					<button
						type="button"
						className="btn btn-outline-dark btn-sm"
						onClick={action}>Yes</button>
					&nbsp;
					<button
						type="button"
						className="btn btn-outline-dark btn-sm"
						onClick={toggle}>No</button>
				</ModalFooter>
			</Modal>
		</Fragment>
	);
}
