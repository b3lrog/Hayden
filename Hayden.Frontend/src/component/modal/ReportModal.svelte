<script lang="ts">
	import { Utility } from "../../data/utility";

	let setBoardId: number;
	let setPostId: number;

	let sending = false;

	export const showModal: (boardId: number, postId: number) => void = (
		boardId: number,
		postId: number,
	) => {
		setBoardId = boardId;
		setPostId = postId;
		jQuery(banUserModal).modal();
	};

	interface ICategory {
		value: number;
		text: string;
	}

	let category: ICategory = null;
	let additionalInfo: string = "";

	const reportCategories: ICategory[] = [
		{ value: 3, text: "This post violates US law" },
		{ value: 2, text: "This post contains lolikon or shota" },
		{ value: 2, text: "This post contains pornography" },
		{ value: 2, text: "This post contains gore" },
		{ value: 1, text: "Other" },
	];

	async function sendReport() {
		sending = true;
		await Utility.PostForm("/makereport", {
			boardId: setBoardId,
			postId: setPostId,
			categoryLevel: category.value,
			additionalInfo: (category.text + "\n" + additionalInfo).trim(),
		});

		sending = false;

		jQuery(banUserModal).modal("hide");

		category = null;
		additionalInfo = '';
	}

	let banUserModal: HTMLDivElement;
</script>

<div
	bind:this={banUserModal}
	class="modal fade"
	tabindex="-1"
	role="dialog"
	aria-labelledby="reportModalLabel"
	aria-hidden="true"
>
	<div class="modal-dialog" role="document">
		<div class="modal-content">
			<div class="modal-header">
				<h5 class="modal-title" id="reportModalLabel">Report post</h5>
				<button
					type="button"
					class="close"
					data-dismiss="modal"
					aria-label="Close"
				>
					<span aria-hidden="true">&times;</span>
				</button>
			</div>
			<div class="modal-body">
				<div class="container">
					<div class="row my-1">
						<div class="col-4">Reason:</div>
						<div class="col-8">
							<select class="form-control" bind:value={category} disabled={sending}>
								{#each reportCategories as category}
									<option value={category}>
										{category.text}
									</option>
								{/each}
							</select>
						</div>
					</div>
					<div class="row my-1">
						<div class="col-4">Additional info:</div>
						<div class="col-8">
							<textarea
								class="form-control"
								bind:value={additionalInfo}
								disabled={sending}
							/>
						</div>
					</div>
				</div>
			</div>
			<div class="modal-footer">
				{#if sending}
					<div class="mr-2 spinner-border spinner-border-sm" role="status">
						<span class="sr-only">Sending...</span>
					</div>
				{/if}
				<button
					type="button"
					class="btn btn-light btn-sm"
					data-dismiss="modal"
					disabled={sending}
				>
					Close
				</button>
				<button
					type="button"
					class="btn btn-danger btn-sm font-weight-bold"
					on:click={sendReport}
					disabled={sending}
				>
					{sending ? 'Sending...' : 'Report'}
				</button>
			</div>
		</div>
	</div>
</div>

<style>
	.modal {
		color: #212529;
	}

	textarea {
		resize: none;
	}
</style>
