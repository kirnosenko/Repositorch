import React, { Fragment, useState } from 'react';
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from 'reactstrap';

export const YesNoButton = (props) => {

	const [modal, setModal] = useState(false);
	const toggle = () => setModal(!modal);
	const action = () => {
		props.yesAction();
		toggle();
	}

	return (
		<Fragment>
			<Button color="danger" size="sm" onClick={toggle}>{props.label}</Button>
			<Modal isOpen={modal} toggle={toggle}>
				<ModalHeader toggle={toggle}>{props.title}</ModalHeader>
				<ModalBody>{props.text}</ModalBody>
				<ModalFooter>
					<Button color="primary" onClick={action}>Yes</Button>{' '}
					<Button color="secondary" onClick={toggle}>No</Button>
				</ModalFooter>
			</Modal>
		</Fragment>
	);
}
