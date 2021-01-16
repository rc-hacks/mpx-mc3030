# MC3030 Tool

The MC3030 Tool is a tool that allows you to access the Multiplex Profi mc3030 from a PC. 

Features include:

- ID the radio.
- Backup and restore single model memories.
- Backup and restore the data RAM.
- Backup and restore the entire RAM.
- Analyse changes to a RAM block.

## Usage

To use this tool, you need to connect the MC3030 to your PC via a USB Serial Convertor. Any USB to TTL UART, such as a FTDI FT232R serial convertor, will do. See **Serial Port Connection** below for details on how to connect the serial cable.

To display the usage of this tool, run `mpx-mc3030` from a command-prompt.

## Downloads

To download a pre-build executable, go to the [releases](https://github.com/rc-hacks/mpx-mc3030/releases) folder.

### Prerequisites

- On Windows, you need Windows 7 or higher with the .NET Framework installed.
- On Linux or macOS, you need Mono to be installed. You can execute **mpx-mc3030** by running `mono mpx-mc3030.exe`.

## Building the software

This software is written in C# using Visual Studio 2019.

## Serial Port Connection

The MC3030 micro controller has a TTL (5V) serial port connected to the radios DIN connector.

The serial port is operated at 9600,N,8,1.

The pinout is:

```pre
     2
  4     5
1    +    3
  6     7
```

- 3: GND
- 4: TX (out)
- 7: RX (in)

## Technical Details

The Profi mc3030 has a battery backed RAM via a CR2450 coin cell, where the radio stores the model data.

Typical RAM sizes are 8KByte and 32KByte, which is able to hold 15 or 99 models.

Besides model data, the RAM also holds firmware data, which includes localized user interface strings, and a number of unknown data. The RAM firmware data cannot be initialized from the ROM, which means the radio requires service when RAM memory corruption occurs. If you suspect a near empty buffer battery, or you indent to replace the buffer battery, be sure to backup the RAM before.

### Memory Layout

Memory is organized in blocks, each 256 bytes in size. The total number of blocks is model dependent.

An 8KByte RAM has 32 blocks, a 32KByte RAM has 128 blocks. Note that the block numbers are modulo the RAM size, i.e. for the 8K RAM, if you address block 112 (0x70), you will access block 16 (0x10).

The last 16 blocks are reserved for firmware data.

Nr. | Description 
----|------------
0-N | N+1 Model memories
112 | Model memories extentions, system data
... | ...
127 | 

### Memory Read Command

To read memory, the following sequence is used:
- PC: Send BLOCK, 0xCF, 0xCF
- Radio: Responds with 0x06 (ready).
- PC: Send 0x14 (send data)
- Radio: Transmits 256 bytes, plus one byte checksum (XOR over 256 bytes)

### Memory Write Command

To write memory, the following sequence is used:
- PC: Send BLOCK, 0x8F, 0x8F
- Radio: Responds with 0x06 (ready).
- PC: Send 256 bytes, plus one byte checksum (XOR over 256 bytes)
- Radio: Responds with 0x14, 0x06 (ready).

## License

[GNU GPLv3](./LICENSE)

Copyright (C) 2018 Marius Greuel.
