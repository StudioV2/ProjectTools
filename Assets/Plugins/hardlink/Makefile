
OUTPUT=hardlink
C_FILES=hardlink.c

all:
	gcc ${C_FILES} -o ${OUTPUT}

clean:
	rm ${OUTPUT}

install: all
	mkdir -p /usr/local/bin && cp ${OUTPUT} /usr/local/bin

