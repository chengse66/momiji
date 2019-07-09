#include <malloc.h>
#include <memory.h>

typedef struct _octal_tree_node {
	int leaf;
	int pixels_count;
	int red;
	int green;
	int blue;
	struct _octal_tree_node *children[8];
	struct _octal_tree_node *next_reducible;
} octal_tree_node;

typedef struct {
	octal_tree_node *root;
	int leaves_count;
	octal_tree_node *reducible_nodes[8];
	int color_bits;
	octal_tree_node *previous_node;
	int previous_color;
} octal_tree;

const int octal_tree_mask[] = { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01, };

octal_tree_node* octal_tree_node_create(int level, octal_tree *proprietor) {
	octal_tree_node *current = calloc(1, sizeof(octal_tree_node));

	current->leaf = (level == proprietor->color_bits);

	if (current->leaf) {
		++proprietor->leaves_count;
	} else {
		current->next_reducible = proprietor->reducible_nodes[level];

		proprietor->reducible_nodes[level] = current;
	}

	return current;
}

void octal_tree_node_construct_palette(unsigned char *palette, int *count, octal_tree_node *current) {
	int index;

	if (current->leaf) {
		index = 3 * *count;

		palette[index + 0] = current->red / current->pixels_count;
		palette[index + 1] = current->green / current->pixels_count;
		palette[index + 2] = current->blue / current->pixels_count;

		++(*count);
	} else {
		for (index = 0; 8 > index; ++index)
			if (current->children[index])
				octal_tree_node_construct_palette(palette, count, current->children[index]);
	}
}

void octal_tree_node_increment(unsigned char *pixel, octal_tree_node *current) {
	current->red = current->red + pixel[2];
	current->green = current->green + pixel[1];
	current->blue = current->blue + pixel[0];

	++current->pixels_count;
}

int octal_tree_node_reduce(octal_tree_node *current) {
	int index, count = 0;

	current->red = 0;
	current->green = 0;
	current->blue = 0;

	for (index = 0; 8 > index; ++index)
		if (current->children[index]) {
			current->red = current->red + current->children[index]->red;
			current->green = current->green + current->children[index]->green;
			current->blue = current->blue + current->children[index]->blue;
			current->pixels_count = current->pixels_count + current->children[index]->pixels_count;

			free(current->children[index]);

			current->children[index] = 0;

			++count;
		}

	current->leaf = 1;

	return count - 1;
}

void octal_tree_node_add_color(unsigned char *pixel, int level, octal_tree_node *current, octal_tree *proprietor) {
	int shift, index;

	if (current->leaf) {
		octal_tree_node_increment(pixel, current);

		proprietor->previous_node = current;
	} else {
		shift = 7 - level;
		index =
			((pixel[2] & octal_tree_mask[level]) >> (shift - 2)) |
			((pixel[1] & octal_tree_mask[level]) >> (shift - 1)) |
			((pixel[0] & octal_tree_mask[level]) >> (shift));

		if (!current->children[index])
			current->children[index] = octal_tree_node_create(level + 1, proprietor);

		octal_tree_node_add_color(pixel, level + 1, current->children[index], proprietor);
	}
}

void octal_tree_reduce(octal_tree *tree) {
	int index;
	octal_tree_node *node;

	for (index = tree->color_bits - 1; 0 < index && !tree->reducible_nodes[index]; --index) ;

	node = tree->reducible_nodes[index];

	tree->reducible_nodes[index] = node->next_reducible;

	tree->leaves_count = tree->leaves_count - octal_tree_node_reduce(node);
	tree->previous_node = 0;
}

void octal_tree_add_color(unsigned char *pixel, octal_tree *tree) {
	if (tree->previous_color == *(int*)pixel && tree->previous_node) {
		octal_tree_node_increment(pixel, tree->previous_node);
	} else {
		tree->previous_color = *(int*)pixel;

		octal_tree_node_add_color(pixel, 0, tree->root, tree);
	}
}

void octal_tree_initial(int color_bits, octal_tree *tree) {
	memset(tree, 0, sizeof(octal_tree));

	tree->color_bits = color_bits;
	tree->root = octal_tree_node_create(0, tree);
}

void octal_tree_destroy(octal_tree_node *current) {
	int index;

	for (index = 0; 8 > index; ++index)
		if (current->children[index])
			octal_tree_destroy(current->children[index]);

	free(current);
}

int octal_tree_generate_palette(int *pixels, int length, unsigned char *palette, unsigned char *back_color, int max_color) {
	octal_tree tree;
	int count = 0;

	octal_tree_initial(8, &tree);

	while (length--)
		octal_tree_add_color((void*)pixels++, &tree);

	while (tree.leaves_count >= max_color)
		octal_tree_reduce(&tree);

	octal_tree_node_construct_palette(palette, &count, tree.root);

	octal_tree_destroy(tree.root);

	palette = palette + 3 * count;

	palette[0] = back_color[2];
	palette[1] = back_color[1];
	palette[2] = back_color[0];

	return count + 1;
}
